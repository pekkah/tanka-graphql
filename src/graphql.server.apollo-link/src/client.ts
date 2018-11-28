import { 
  FetchResult, 
  Observable, 
  Operation 
} from "apollo-link";

import {
  HubConnection,
  HubConnectionBuilder,
  IHttpConnectionOptions
} from "@aspnet/signalr";

import { Request } from "./request";
import { Subscription } from "./subscription";

export class FuguClient {
  private hub: HubConnection;
  private started: Promise<void> = undefined;

  constructor(private url: string, private options?: IHttpConnectionOptions) {
    const builder = new HubConnectionBuilder();

    if (options) {
      builder.withUrl(url, options);
    } else {
      builder.withUrl(url);
    }

    this.hub = builder.build();
    this.started = new Promise<void>(resolve => {
      this.hub.start().then(() => {
        resolve();
      });
    });
  }

  public request(operation: Operation): Observable<FetchResult> {
    return new Observable<FetchResult>(subscriber => {
      const sub = this.query(operation);
      sub.source.observable.subscribe(
        next => {
          subscriber.next({
            data: next.data,
            errors: next.errors,
            extensions: next.extensions
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

    this.started.then(() => {
      const stream = this.hub.stream("query", new Request(operation));
      sub.subscribe(stream);
    });

    return sub;
  }
}
