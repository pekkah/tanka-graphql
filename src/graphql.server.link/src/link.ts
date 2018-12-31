import {
  ApolloLink,
  FetchResult,
  NextLink,
  Observable,
  Operation
} from "apollo-link";
import { TankaClient } from ".";

export class TankaLink extends ApolloLink {
  constructor(private client: TankaClient) {
    super();
  }

  public request(
    operation: Operation,
    forward?: NextLink
  ): Observable<FetchResult> | null {
    return this.client.request(operation);
  }
}
