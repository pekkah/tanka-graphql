import { GraphQLError } from "graphql";

export class ExecutionResult {
  public errors?: ReadonlyArray<GraphQLError>;
  public data?: Record<string, any>;
  public extensions?: Record<string, any>;
}
