import * as React from "react";
import { Provider } from "react-redux";
import { store, Playground } from "graphql-playground-react";
import { Session } from "graphql-playground-react/lib/state/sessions/reducers";
import { TankaClient, TankaLink } from "@tanka/tanka-graphql-server-link";
import { LogLevel } from "@aspnet/signalr";
import { ISettings } from 'graphql-playground-react/lib/types';
import { RetryLink } from "apollo-link-retry";
import { ApolloLink } from 'apollo-link';
import { ClientOptions } from '@tanka/tanka-graphql-server-link/dist/client';

var options: ClientOptions = {
  connection: {
    logger: LogLevel.Information
  },
  reconnectAttempts: Infinity,
  reconnectInitialWaitMs: 5000,
  reconnectAdditionalWaitMs: 2000
};

var client = new TankaClient("https://localhost:5000/graphql", options);
var tankaLink = new TankaLink(client);

const link = ApolloLink.from([
  new RetryLink(),
  tankaLink
]);

var settings: ISettings  = {
  'editor.cursorShape': 'line', // possible values: 'line', 'block', 'underline'
  'editor.fontFamily': `'Source Code Pro', 'Consolas', 'Inconsolata', 'Droid Sans Mono', 'Monaco', monospace`,
  'editor.fontSize': 14,
  'editor.reuseHeaders': true, // new tab reuses headers from last tab
  'editor.theme': 'dark', // possible values: 'dark', 'light'
  'general.betaUpdates': false,
  'prettier.printWidth': 80,
  'prettier.tabWidth': 2,
  'prettier.useTabs': false,
  'request.credentials': 'omit', // possible values: 'omit', 'include', 'same-origin'
  'schema.polling.enable': true, // enables automatic schema polling
  'schema.polling.endpointFilter': '*', // endpoint filter for schema polling
  'schema.polling.interval': 10000, // schema polling interval in ms
  'schema.disableComments': false,
  'tracing.hideTracingResponse': true,
};

class App extends React.Component {
  createLink = (session: Session) => {
    return {
      link: link
    };
  };

  public render() {
    return (
      <Provider store={store}>
        <Playground createApolloLink={this.createLink} settings={settings} />
      </Provider>
    );
  }
}

export default App;
