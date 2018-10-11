import {
  ApolloLink,
  FetchResult,
  NextLink,
  Observable,
  Operation
} from "apollo-link";

import {
  HubConnection,
  HubConnectionBuilder,
  IStreamResult,
  IStreamSubscriber,
  LogLevel
} from "@aspnet/signalr";

import PushStream from "zen-push";
import { OperationMessage } from "./message";
import { Request } from "./request";

export class Client implements IStreamSubscriber<OperationMessage> {
  public closed?: boolean;
  private hub: HubConnection;
  private stream: IStreamResult<OperationMessage>;
  private subject: PushStream<OperationMessage>;
  private nextOperationId: number = 0;
  private target: PushStream<Request>;

  constructor(private url: string) {
    this.hub = new HubConnectionBuilder()
      .withUrl(url)
      .configureLogging(LogLevel.Information)
      .build();

    this.subject = new PushStream<OperationMessage>();
    this.target = new PushStream<Request>();
    this.connect();
  }

  public request(operation: Operation): Observable<FetchResult> {
    console.log(`Client: ${operation}`);
    return new Observable<FetchResult>(subscriber => {
      const opId = this.start(operation);
      const sub = this.subject.observable
        .filter(fr => fr.id === opId)
        .subscribe(
          next => {
            switch(next.type) {
              case "complete":
                this.execute(opId, "stop");
                subscriber.complete();
                break;
              case "data":
                var data = {
                  data: next.payload.Data,
                  errors: next.payload.Errors,
                  extensions: next.payload.Extension
                };
                subscriber.next(data);
                break;
            }
          },
          error => subscriber.error(error),
          () => subscriber.complete()
        );

      return () => {
        sub.unsubscribe();
        // todo: unsub from server
      };
    });
  }

  public next(value: OperationMessage): void {
    this.subject.next(value);
  }

  public error(err: any): void {
    this.subject.error(err);
  }

  public complete(): void {
    this.subject.complete();
  }

  private async connect(): Promise<boolean> {
    await this.hub.start()
      .catch(err => console.error(err.toString()));

    this.stream = this.hub.stream<OperationMessage>("Connect");

    // connect stream to subject
    this.stream.subscribe(this);
    this.target.observable.subscribe(next => {
      console.log("Request:", next);
      this.hub.invoke("Execute", next);
    });
    return true;
  }

  private start(operation: Operation): string {
    const id = this.nextId();
    this.target.next(new Request(id, "start", operation));
    return id;
  }

  private execute(id: string, type: string): string {
    this.target.next(new Request(id, type, null));
    return id;
  }

  private nextId(): string {
    return String(++this.nextOperationId);
  }
}
