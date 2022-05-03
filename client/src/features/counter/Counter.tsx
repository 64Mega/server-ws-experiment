// Server-controlled RTC counter demo
import { useEffect, useState } from "react";
import { WS_DEFAULT_SERVER } from "../../constants/webSockets";
import { ServerSocket } from "../../socket/ServerSocket";

const serverSocket = new ServerSocket(WS_DEFAULT_SERVER);

export const Counter = () => {
	const [count, setCount] = useState(0);

	useEffect(() => {
		const handleCounterMessage = (data) => {
			setCount(data.value);
		};

		serverSocket.addActionListener("COUNTER", handleCounterMessage);

		return () => {
			serverSocket.removeActionListener("COUNTER", handleCounterMessage);
		};
	}, []);

	return (
		<div>
			<strong>Counter value from Server:</strong> {count}
		</div>
	);
};
