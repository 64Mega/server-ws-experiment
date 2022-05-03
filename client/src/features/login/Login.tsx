import { useState } from "react";
import { API_SERVER } from "../../constants/webSockets";
import { useAuth } from "../../hooks/useAuth";

export const Login = () => {
	const [state, setState] = useState({
		email: "",
		password: "",
	});
	const [error, setError] = useState("");
	const auth = useAuth();

	const handleInput = (field) => (ev) => {
		setState({ ...state, [field]: ev.target.value });
	};

	const login = async (ev) => {
		ev.preventDefault();
		ev.stopPropagation();
		if (!state.email || !state.password) {
			return;
		}

		const payload = JSON.stringify(state);

		const result = await fetch(`${API_SERVER}/user/login`, {
			body: payload,
			method: "POST",
			mode: "cors",
			headers: {
				"Content-Type": "application/json",
			},
		});

		if (result && result.status === 200 && result.body) {
			const response = await result.json();
			auth.updateJwt(response.token);
			window.location.reload();
		}
	};

	return (
		<div>
			<fieldset style={{ width: "max-content" }}>
				<legend>Login</legend>
				<form action="#" onSubmit={login}>
					<div
						style={{
							display: "flex",
							flexDirection: "column",
							width: "max-content",
							gap: "1rem",
						}}
					>
						{error && (
							<strong style={{ color: "red" }}>{error}</strong>
						)}
						<span
							style={{
								display: "flex",
								gap: "2rem",
								justifyContent: "space-between",
							}}
						>
							<label htmlFor="login-email">Email:</label>
							<input
								type="email"
								id="login-email"
								required
								autoComplete="current-email"
								onInput={handleInput("email")}
								value={state.email}
							/>
						</span>
						<span
							style={{
								display: "flex",
								gap: "2rem",
								justifyContent: "space-between",
							}}
						>
							<label htmlFor="login-password">Password:</label>
							<input
								type="password"
								id="login-password"
								required
								autoComplete="current-password"
								onInput={handleInput("password")}
								value={state.password}
							/>
						</span>
						<button
							type="submit"
							disabled={!state.email || !state.password}
						>
							Login
						</button>
					</div>
				</form>
			</fieldset>
		</div>
	);
};
