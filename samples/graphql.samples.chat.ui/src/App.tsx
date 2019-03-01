import * as React from "react";
import { Provider } from "react-redux";
import { store, Playground } from "graphql-playground-react";
import { Session } from "graphql-playground-react/lib/state/sessions/reducers";
import { TankaClient, TankaLink } from "@tanka/tanka-graphql-server-link";
import { IHttpConnectionOptions, LogLevel } from "@aspnet/signalr";

var options: IHttpConnectionOptions = {
  logger: LogLevel.Information
};

var client = new TankaClient("https://localhost:5000/graphql", options);
var link = new TankaLink(client);

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
