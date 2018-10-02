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
  ISubscription
} from "@aspnet/signalr"

import {
  createSubject,
} from "light-observable/observable"

import {
  Observable as InputObservable,
  SubscriptionObserver as InputObserver
} from 'light-observable'

export class Client {
  private hub: HubConnection;

  constructor(private url: string) {
    this.hub = new HubConnectionBuilder()
      .withUrl(this.url)
      .build();
  }

  public request(operation: Operation): Observable<FetchResult> {
    var stream = this.execute(operation);
    var subscription = new Subscription(stream);
    return subscription.getObservable();
  }

  private execute(operation: Operation): IStreamResult<FetchResult> {
    return this.hub.stream<FetchResult>("subscribe", operation);
  }
}

class Subscription implements IStreamSubscriber<FetchResult> {
  closed?: boolean;

  next(value: FetchResult): void {
    this.sink.next(value);
  }

  error(err: any): void {
    this.sink.error(err);
    this.closed = true;
  }

  complete(): void {
    this.sink.complete();
    this.closed = true;
  }

  private observable: InputObservable<FetchResult>;
  private sink: InputObserver<FetchResult>;
  private streamSubscription: ISubscription<FetchResult>

  constructor(private stream: IStreamResult<FetchResult>) {
    [this.observable, this.sink] = createSubject();
    this.streamSubscription = this.stream.subscribe(this);
  }



  public getObservable(): Observable<FetchResult> {
    return new Observable<FetchResult>(subscriber => {
      var sub = this.observable.subscribe(
        next => subscriber.next(next),
        err => subscriber.error(err),
        () => subscriber.complete()
      )

      return () => {
        sub.unsubscribe();
        this.streamSubscription.dispose();
      }
    })
  }
}
