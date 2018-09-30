import {
  ApolloLink,
  FetchResult,
  NextLink,
  Observable,
  Operation
} from "apollo-link";

export class Client {
  private application: Observable<FetchResult>;

  constructor(private url: string) {}

  public request(operation: Operation): Observable<FetchResult> {
    this.executeOperation(operation);
    return this.application;
  }

  private executeOperation(operation: Operation): any {
    throw new Error("Method not implemented.");
  }
}
