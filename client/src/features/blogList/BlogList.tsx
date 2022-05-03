import { useState, useEffect } from "react";
import { API_SERVER, WS_DEFAULT_SERVER } from "../../constants/webSockets";
import { ServerSocket } from "../../socket/ServerSocket";

const server = new ServerSocket(WS_DEFAULT_SERVER);

export const BlogList = () => {
	const [blogs, setBlogs] = useState([]);

	useEffect(() => {
		const fetchBlogs = async () => {
			const response = await fetch(`${API_SERVER}/blogs`);
			if (response.body) {
				setBlogs(await response.json());
			}
		};

		const handleLiveNotification = () => {
			fetchBlogs();
		};
		fetchBlogs();
		server.addActionListener("BLOGPOST", handleLiveNotification);

		return () => {
			server.removeActionListener("BLOGPOST", handleLiveNotification);
		};
	}, []);

	return (
		<div
			style={{ display: "flex", flexDirection: "column", gap: "0.8rem" }}
		>
			{blogs.map((blog, index) => {
				return (
					<div
						key={index}
						style={{
							border: "1px solid #aaa",
							boxShadow: "1px 1px 2px rgba(0,0,0,0.2)",
							padding: "1.2rem",
							display: "flex",
							flexDirection: "column",
						}}
					>
						<strong>{blog.title}</strong>
						<p>{blog.body}</p>
						<em>Written by {blog.user?.name}</em>
					</div>
				);
			})}
		</div>
	);
};
