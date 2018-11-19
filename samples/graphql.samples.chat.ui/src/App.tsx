import * as React from "react";
import { Provider } from "react-redux";
import { store, Playground } from "graphql-playground-react";
import { Session } from "graphql-playground-react/lib/state/sessions/reducers";
import { Client, SignalrLink } from "@fugu-fw/fugu-graphql-server-link";
import { IHttpConnectionOptions, LogLevel } from "@aspnet/signalr";

var options: IHttpConnectionOptions = {
  accessTokenFactory: () => "123123",
  logMessageContent: true,
  logger: LogLevel.Trace
};

var client = new Client("https://localhost:5000/graphql", options);
var link = new SignalrLink(client);

class App extends React.Component {
  createLink = (session: Session) => {
    return {
      link: link
    };
  };

  public render() {
    return (
      <Provider store={store}>
        <Playground createApolloLink={this.createLink} />
      </Provider>
    );
  }
}

export default App;
