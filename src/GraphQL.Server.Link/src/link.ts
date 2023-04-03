import { ApolloLink, FetchResult, Observable, Operation } from "@apollo/client";
import { TankaClient } from ".";

export class TankaLink extends ApolloLink {
  constructor(private client: TankaClient) {
    super();
  }

  public request(operation: Operation): Observable<FetchResult> | null {
    return this.client.request(operation);
  }
}
