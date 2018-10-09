import { FetchResult } from "apollo-link";

export class OperationMessage {
  public id: string;
  public type: string;
  public payload: any;
}
