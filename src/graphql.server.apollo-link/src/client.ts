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
  private starting: Promise<void> = undefined;

  constructor(private url: string, private options?: IHttpConnectionOptions) {
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

  public async connect(): Promise<void> {
    if (this.starting != undefined) {
      return this.starting;
    }

    this.starting = this.hub.start()
      .catch(err => {
        console.log("Error starting hub", err);
        throw err;
      });

    return this.starting;
  }
}
