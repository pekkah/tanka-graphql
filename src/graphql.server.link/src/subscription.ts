import {
  IStreamResult,
  IStreamSubscriber,
  ISubscription
} from "@microsoft/signalr";

import PushStream from "zen-push";

import { ExecutionResult } from "./execution-result";

export class Subscription implements IStreamSubscriber<ExecutionResult> {
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
  }
  public complete(): void {
    this.source.complete();
    this.closed = true;
  }

  public dispose() {
    if (this.sub) {
      try {
        this.sub.dispose();
      } catch (e) {
        // noop
      } finally {
        this.sub = null;
      }
    }
  }
}
