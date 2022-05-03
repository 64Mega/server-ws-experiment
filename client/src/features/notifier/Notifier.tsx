import { useState, useEffect } from "react";
import { WS_DEFAULT_SERVER } from "../../constants/webSockets";
import { ServerSocket } from "../../socket/ServerSocket";

const server = new ServerSocket(WS_DEFAULT_SERVER);

export const Notifier = () => {
	const [notifications, setNotifications] = useState([]);

	useEffect(() => {
		const handleServerNotify = (data) => {
			notifications.unshift(
				`[${Date.now().toLocaleString()}]ALERT! A new item of type ${
					data.action
				} was added!`
			);
			setNotifications([...notifications].slice(0, 10));
		};

		server.addActionListener("BLOGPOST", handleServerNotify);

		return () => {
			server.removeActionListener("BLOGPOST", handleServerNotify);
		};
	}, []);

	return (
		<ul>
			{notifications.map((notification, index) => (
				<li key={index}>{notification}</li>
			))}
		</ul>
	);
};
