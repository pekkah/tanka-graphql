import { TankaClient, TankaLink } from ".";

describe("exports", () => {
  test("Client", () => {
    expect(TankaClient).toBeDefined();
  });

  test("SignalrLink", () => {
    expect(TankaLink).toBeDefined();
  });
});
