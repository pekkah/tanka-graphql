import {
  ApolloLink,
  FetchResult,
  NextLink,
  Observable,
  Operation
} from "apollo-link";
import { FuguClient } from ".";

export class FuguLink extends ApolloLink {
  constructor(private client: FuguClient) {
    super();
  }

  public request(
    operation: Operation,
    forward?: NextLink
  ): Observable<FetchResult> | null {
    return this.client.request(operation);
  }
}
