import { Client, SignalrLink } from "../.";

describe("exports", () => {
  test("Client", () => {
    expect(Client).toBeDefined();
  });

  test("SignalrLink", () => {
    expect(SignalrLink).toBeDefined();
  });
});
