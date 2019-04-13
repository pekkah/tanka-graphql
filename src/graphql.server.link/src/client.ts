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

export class ClientOptions {
  connection: IHttpConnectionOptions;
  reconnectAttempts: number = 10;
  reconnectInitialWaitMs: number = 1000;
  reconnectAdditionalWaitMs: number = 500;
}

export class TankaClient {
  private BACKGROUND_QUEUE_TIMER_MS = 1000;
  private hub: HubConnection;
  private connected: boolean = false;
  private connecting: boolean = false;
  private buffer: { sub: Subscription, op: Operation }[] = [];
  private backgroundQueueTimerId: NodeJS.Timeout;
  private reconnectTimerId: NodeJS.Timeout;

  constructor(private url: string, private options: ClientOptions) {
    const builder = new HubConnectionBuilder();

    if (options && options.connection) {
      builder.withUrl(url, options.connection);
    } else {
      builder.withUrl(url);
    }

    this.hub = builder.build();
    this.hub.onclose(() => this.onClosed);
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
      catch (e) {
        this.onClosed(e);
        this.queue(sub, operation);
      }
    } else {
      this.start(() => { });
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
      catch (e) {
        this.onClosed(e);
        this.queue(sub, op);
      }
    }
  }

  private reconnect() {
    let count = 0;
    this.reconnectTimerId = setInterval(() => {
      this.start(() => {
        clearInterval(this.reconnectTimerId);
        console.log(`Connected`)
      });

      if (this.connected) {
        return;
      }

      count++;
      console.log(`Connection attempt #${count}`);

      if (count >= this.options.reconnectAttempts) {
        clearInterval(this.reconnectTimerId);
        this.onClosed();
      }
    }, this.options.reconnectInitialWaitMs + (count * this.options.reconnectAdditionalWaitMs));
  }

  private start(callback: () => void) {
    if (this.connected) {
      callback();
      return;
    }

    if (this.connecting) {
      return;
    }

    this.connecting = true;
    return this.hub.start().then(() => {
      this.connected = true;
      this.connecting = false;
      this.backgroundQueueTimerId = setInterval(() => this.processQueue(), this.BACKGROUND_QUEUE_TIMER_MS);
    })
      .catch(err => {
        this.connected = false;
        this.connecting = false;
        throw err;
      })
  }

  private onClosed(error?: Error) {
    this.connected = false;

    if (this.backgroundQueueTimerId) {
      clearInterval(this.backgroundQueueTimerId);
    }

    if (error) {
      console.log("Connection closed due to error", error);
      this.reconnect();
    }
    else {
      console.log("Connection closed.");
      if (this.reconnectTimerId) {
        clearInterval(this.reconnectTimerId);
      }
    }
  }
}
