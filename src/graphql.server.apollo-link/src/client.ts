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
  IStreamSubscriber
} from "@aspnet/signalr";

import PushStream from "zen-push";
import { OperationMessage } from "./message";
import { Request } from "./request";

export class Client implements IStreamSubscriber<FetchResult> {
  public closed?: boolean;
  private hub: HubConnection;
  private stream: IStreamResult<OperationMessage>;
  private subject: PushStream<any>;
  private nextOperationId: number;

  constructor(private url: string) {
    this.hub = new HubConnectionBuilder().withUrl(this.url).build();
    this.stream = this.connect();
    this.subject = new PushStream<OperationMessage>();

    // connect stream to subject
    this.stream.subscribe(this);
  }

  public request(operation: Operation): Observable<FetchResult> {
    return new Observable<FetchResult>(subscriber => {
      const opId = this.execute(operation);
      const sub = this.subject.observable
        .filter(fr => fr.id === opId)
        .subscribe(
          next => subscriber.next(next.payload),
          error => subscriber.error(error),
          () => subscriber.complete()
        );

      return () => {
        sub.unsubscribe();
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

  private connect(): IStreamResult<OperationMessage> {
    return this.hub.stream<OperationMessage>("Connect");
  }

  private execute(operation: Operation): string {
    const id = this.nextId();
    this.hub.invoke("execute", new Request(id, operation));
    return id;
  }

  private nextId(): string {
    return String(++this.nextOperationId);
  }
}
