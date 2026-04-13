import * as signalR from "@microsoft/signalr";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

export type NotificationEvent =
  | { type: "JobStatusChanged"; data: { jobId: string; agencyId: string; status: string; agentType?: string; timestamp: string } }
  | { type: "ContentGenerated"; data: { contentId: string; agencyId: string; title: string; status: string; timestamp: string } }
  | { type: "ContentApproved"; data: { contentId: string; agencyId: string; title: string; timestamp: string } }
  | { type: "ContentRejected"; data: { contentId: string; agencyId: string; title: string; timestamp: string } }
  | { type: "PublishResult"; data: { contentId: string; agencyId: string; platform: string; success: boolean; postUrl?: string; timestamp: string } };

export class NotificationClient {
  private connection: signalR.HubConnection | null = null;
  private startPromise: Promise<void> | null = null;
  private listeners: ((event: NotificationEvent) => void)[] = [];

  async connect(accessToken: string) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) return;
    if (this.startPromise) return this.startPromise;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/notifications`, {
        accessTokenFactory: () => accessToken,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();

    this.connection = connection;

    const events = ["JobStatusChanged", "ContentGenerated", "ContentApproved", "ContentRejected", "PublishResult"] as const;
    for (const eventName of events) {
      connection.on(eventName, (data) => {
        const event = { type: eventName, data } as NotificationEvent;
        this.listeners.forEach((fn) => fn(event));
      });
    }

    this.startPromise = connection.start().catch((err) => {
      // StrictMode double-mount aborts the first negotiation; ignore that specific race.
      if (!String(err?.message ?? err).includes("stopped during negotiation")) {
        console.error("SignalR connection failed:", err);
      }
    }).finally(() => {
      this.startPromise = null;
    });

    return this.startPromise;
  }

  async joinAgency(agencyId: string) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke("JoinAgencyGroup", agencyId);
    }
  }

  async leaveAgency(agencyId: string) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke("LeaveAgencyGroup", agencyId);
    }
  }

  onNotification(callback: (event: NotificationEvent) => void) {
    this.listeners.push(callback);
    return () => {
      this.listeners = this.listeners.filter((fn) => fn !== callback);
    };
  }

  async disconnect() {
    const conn = this.connection;
    if (conn) {
      try {
        if (this.startPromise) await this.startPromise;
        await conn.stop();
      } catch {
        // ignore
      }
      this.connection = null;
    }
    this.listeners = [];
  }
}

export const notificationClient = new NotificationClient();
