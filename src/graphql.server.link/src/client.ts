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

export class TankaClient {
  private hub: HubConnection;
  private connected: boolean = false;
  private connecting: boolean = false;
  private buffer: { sub: Subscription, op: Operation }[] = [];
  private backgroundQueueTimerId: NodeJS.Timeout;

  constructor(private url: string, private options?: IHttpConnectionOptions) {
    const builder = new HubConnectionBuilder();

    if (options) {
      builder.withUrl(url, options);
    } else {
      builder.withUrl(url);
    }

    this.hub = builder.build();
    this.hub.onclose(()=> this.onClosed);
    this.backgroundQueueTimerId = setInterval(()=> this.processQueue(), 10000);
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

    if (this.connected) {
      try {
        const stream = this.hub.stream("query", new Request(operation));
        sub.subscribe(stream);
      }
      catch(e) {
        this.connected = false;
        this.queue(sub, operation);
      }
    } else {
      this.reconnect();
      this.queue(sub, operation);
    }

    return sub;
  }

  private queue(subscription: Subscription, operation: Operation) {
    this.buffer.push({ sub: subscription, op: operation });
  }

  private processQueue() {
    while (this.buffer.length > 0) {
      if (!this.connected)
        break;

      const { sub, op } = this.buffer.pop();
      try {
        const stream = this.hub.stream("query", new Request(op));
        sub.subscribe(stream);
      }
      catch(e) {
        this.connected = false;
        this.queue(sub, op);
      }
    }
  }

  private reconnect() {
    const timer = setInterval(()=> {
      this.start(()=> clearInterval(timer));
    }, 1000);
  }

  private start(callback: () => void) {
    if (this.connected) {
      return;
    }

    if (this.connecting) {
      return;
    }

    this.connecting = true;
    return this.hub.start().then(() => {
      this.connected = true;
      this.connecting = false;
      this.processQueue();
    })
      .catch(err => {
        this.connected = false;
        this.connecting = false;
        throw err;
      })
  }

  private onClosed(error?: Error) {
    this.connected = false;

    if (error) {
      console.log("Connection closed due to error", error);
      this.reconnect();
    }
    else {
      console.log("Connection closed.");
    }
  }
}
