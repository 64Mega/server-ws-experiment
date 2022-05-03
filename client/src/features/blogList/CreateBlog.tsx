import { useState } from "react";
import { API_SERVER, WS_DEFAULT_SERVER } from "../../constants/webSockets";
import { useAuth } from "../../hooks/useAuth";
import { ServerSocket } from "../../socket/ServerSocket";

const server = new ServerSocket(WS_DEFAULT_SERVER);
const defaultState = {
	title: "",
	body: "",
};

export const CreateBlog = () => {
	const auth = useAuth();
	const [state, setState] = useState(defaultState);

	const handleInput = (field) => (ev) => {
		setState({ ...state, [field]: ev.target.value });
	};

	const postBlog = async (ev) => {
		ev.preventDefault();
		ev.stopPropagation();
		if (!state.title || !state.body || state.body.length > 200) {
			return;
		}
		await fetch(`${API_SERVER}/blog/post`, {
			method: "POST",
			body: JSON.stringify(state),
			headers: {
				"Content-Type": "application/json",
				Authorization: `Bearer ${auth.jwt}`,
			},
		});

		setState(defaultState);
	};

	return (
		<div>
			<form action="#" onSubmit={postBlog}>
				<div
					style={{
						display: "flex",
						flexDirection: "column",
						gap: "0.5rem",
						width: "100%",
					}}
				>
					<label htmlFor="blog-title">Blog Title:</label>
					<input
						required
						id="blog-title"
						value={state.title}
						onInput={handleInput("title")}
					/>

					<label htmlFor="blog-body">Blog Contents:</label>
					<textarea
						required
						cols={10}
						maxLength={200}
						id="blog-title"
						value={state.body}
						onChange={handleInput("body")}
						style={{ width: "100%", height: "5rem" }}
					/>
					<button
						type="submit"
						disabled={
							!state.title ||
							!state.body ||
							state.body.length > 200
						}
					>
						Post!
					</button>
				</div>
			</form>
		</div>
	);
};
