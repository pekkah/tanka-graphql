import { Operation } from "@apollo/client";
import { print } from "graphql";

export class Request {
  public query: string;
  public variables: Record<string, any>;
  public operationName: string;
  public extensions: Record<string, any>;

  constructor(operation: Operation) {
    this.variables = operation.variables;
    this.operationName = operation.operationName;
    this.extensions = operation.extensions;
    this.query = print(operation.query);
  }
}
