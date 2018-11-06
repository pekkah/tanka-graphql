import { GraphQLError } from "graphql";

export class ExecutionResult {
  public errors?: ReadonlyArray<GraphQLError>;
  public data?: Record<string, any>;
}

export class QueryError {
  public message: string;
}
