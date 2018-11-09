import { FetchResult, Observable, Operation } from "apollo-link";

import {
  HubConnection,
  HubConnectionBuilder,
  IHttpConnectionOptions
} from "@aspnet/signalr";

import { Request } from "./request";
import { Subscription } from "./subscription";

export class Client {
  private hub: HubConnection;
  private connected: boolean;

  constructor(private url: string, private options?: IHttpConnectionOptions) {
    this.connected = false;
    const builder = new HubConnectionBuilder();

    if (options) {
      builder.withUrl(url, options);
    } else {
      builder.withUrl(url);
    }

    this.hub = builder.build();
  }

  public request(operation: Operation): Observable<FetchResult> {
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
          subscriber.error(err);
        },
        () => {
          subscriber.complete();
        }
      );

      return () => sub.dispose();
    });
  }

  public query(operation: Operation): Subscription {
    const sub = new Subscription();

    this.connect().then(() => {
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
    await this.hub.start().catch(err => {
      this.connected = false;
    });

    this.connected = true;
    return true;
  }
}
