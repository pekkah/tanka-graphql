import {
  ApolloLink,
  FetchResult,
  NextLink,
  Observable,
  Operation,
  fromPromise
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
  private connected: boolean;

  constructor(private url: string) {
    this.connected = false;
    this.hub = new HubConnectionBuilder()
      .withUrl(url)
      .configureLogging(LogLevel.Debug)
      .build();
  }

  public request(operation: Operation) : Observable<FetchResult> {
    return new Observable<FetchResult>(subscriber => {
      const sub = this.query(operation);
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

      return ()=> sub.dispose();
    });
  }

  public query(operation: Operation) : Subscription {
    const sub = new Subscription();

    this.connect().then(()=> {
      const stream = this.hub.stream("query", new Request(operation));
      sub.subscribe(stream);
    });

    return sub;
  }

  public async connect(): Promise<boolean> {
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

  constructor() {
    this.source = new PushStream<ExecutionResult>();
  }

  public subscribe(stream: IStreamResult<ExecutionResult>) {
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
    console.log(`Disposing ${this}`);
    this.sub.dispose();
  }
}
