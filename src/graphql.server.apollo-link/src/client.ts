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
  ISubscription,
  LogLevel
} from "@aspnet/signalr";

import PushStream from "zen-push";
import { ExecutionResult } from "./message";
import { Request } from "./request";

export class Client {
  private hub: HubConnection;
  private connected: boolean

  constructor(private url: string) {
    this.connected = false;
    this.hub = new HubConnectionBuilder()
      .withUrl(url)
      .configureLogging(LogLevel.Debug)
      .build();
  }

  public request(operation: Operation): Observable<FetchResult> {
    console.log(`Request: ${operation}`);
    return new Observable<FetchResult>(subscriber => {
      this.connect().then(() => {
        const stream = this.hub.stream("query", new Request(operation));
        const sub = new Subscription(stream);

        sub.source.observable.subscribe(
          next => {
            subscriber.next({
              data: next.data,
              errors: next.errors
            });
          },
          err => {
            console.log(`Error: ${err}`);
            subscriber.error(err);
          },
          () => {
            console.log("Completed");
            subscriber.complete();
          }
        );

        return () => {
          sub.dispose();
        };
      });
    });
  }

  private async connect(): Promise<boolean> {
    if (this.connected) {
      return true;
    }

    this.connected = true;
    console.log("Starting hub");
    await this.hub.start().catch(err => {
      console.error(err.toString());
      this.connected = false;
    });

    console.log("Hub started");
    this.connected = true;
    return true;
  }
}

class Subscription implements IStreamSubscriber<ExecutionResult> {
  public closed?: boolean;
  public source: PushStream<ExecutionResult>;

  private sub: ISubscription<ExecutionResult>;

  constructor(private stream: IStreamResult<ExecutionResult>) {
    this.source = new PushStream<ExecutionResult>();
    this.sub = stream.subscribe(this);
  }

  public next(value: ExecutionResult): void {
    this.source.next(value);
  }
  public error(err: any): void {
    this.source.error(err);
    this.closed = true;
  }
  public complete(): void {
    this.source.complete();
    this.closed = true;
  }

  public dispose() {
    this.sub.dispose();
  }
}
