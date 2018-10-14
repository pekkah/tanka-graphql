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
  LogLevel,
  MessageType
} from "@aspnet/signalr";

import PushStream from "zen-push";
import { OperationMessage } from "./message";
import { Request } from "./request";

export class Client implements IStreamSubscriber<OperationMessage> {
  public closed?: boolean;
  private hub: HubConnection;
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
    console.log(`Request: ${operation}`);
    return new Observable<FetchResult>(subscriber => {
      const opId = this.start(operation);
      const sub = this.subject.observable
        .filter(fr => fr.id === opId)
        .subscribe(
          next => {
            switch(next.type) {
              case "complete":
                this.stop(opId);
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
        console.log(`Unsubscribe: ${opId}`);
        sub.unsubscribe();
        this.stop(opId);
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
    this.hub.on("Data", data => this.subject.next(data))

    await this.hub.start()
    .catch(err => console.error(err.toString()));

    this.target.observable.subscribe(next => {
      console.log("Message:", next);

      switch (next.type) {
        case "start": this.hub.invoke("Start", next);
          break;
        case "stop": this.hub.invoke("Stop", next.id);
          break;
      }      
    });
    return true;
  }

  private start(operation: Operation): string {
    const id = this.nextId();
    this.target.next(new Request(id, "start", operation));
    return id;
  }

  private stop(id: string): string {
    this.target.next(new Request(id, "stop", null));
    return id;
  }

  private nextId(): string {
    return String(++this.nextOperationId);
  }
}
