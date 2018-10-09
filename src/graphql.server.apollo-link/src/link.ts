import {
  ApolloLink,
  FetchResult,
  NextLink,
  Observable,
  Operation
} from "apollo-link";
import { Client } from ".";

export class SignalrLink extends ApolloLink {
  constructor(private client: Client) {
    super();
  }

  public request(
    operation: Operation,
    forward?: NextLink
  ): Observable<FetchResult> | null {
    console.log(`Link: ${operation}`);
    return this.client.request(operation);
  }
}
