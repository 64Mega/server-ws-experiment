// Singleton - we're only listening to one server and only expect one connection at a time per server class.
type ListenerCallback = (data?: any) => void;

export class ServerSocket {
	private static instance : ServerSocket = null;
	private _socket : WebSocket = null;
	private _listeners : {[key: string]: ListenerCallback[]} = {};

	public get isConnected() {
		return this._socket.readyState === WebSocket.OPEN;
	}

	constructor(uri: string) {
		if(!ServerSocket.instance) {
			console.log(`ServerSocket:constructor`,`Connecting to server.`);
			this._socket = new WebSocket(uri);
			this._socket.addEventListener('message', this.handleMessage);
			
			this._socket.addEventListener('close', () => {
				console.log(`ServerSocket:constructor`,`Socket connection closed.`);
			});
			
			this._socket.addEventListener('error', (err) => {
				console.error(`ServerSocket:constructor`,`WebSocket Error:`, err);
			});
			
			this.addActionListener("PING", this.pingListener);

			ServerSocket.instance = this;
			return this;
		} else {
			return ServerSocket.instance;
		}
	}

	private handleMessage = (ev) => {
		const data = JSON.parse(ev.data);
		const action = data.action;
		if(this._listeners[action]) {
			for(const listener of this._listeners[action]) {
				if(listener) {
					listener(data);
				}
			}
		} else {
			console.debug(`ServerSocket:handleMessage`,`Unhandled action type received: ${action}`);
		}
	}

	private pingListener = () => {
		this.sendObjectMessage("PONG",{});
	}

	sendObjectMessage(action: string, obj: any) {
		if(!this.isConnected) { return; }
		const payload = JSON.stringify({action, ...obj});
		this._socket.send(payload);
	}

	addActionListener(action: string, listener: ListenerCallback) {
		if(!this._listeners[action]) {
			this._listeners[action] = [];
		}
		this._listeners[action].push(listener);
	}

	removeActionListener(action: string, listener: ListenerCallback) {
		if(this._listeners[action]) {
			this._listeners[action] = this._listeners[action].filter(entry => entry !== listener);
		}
	}
}