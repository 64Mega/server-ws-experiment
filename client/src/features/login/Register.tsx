// Basic registration form.

import { useState } from "react";
import { API_SERVER } from "../../constants/webSockets";

const initialState = {
	username: "",
	email: "",
	password: "",
};

export const Register = () => {
	const [state, setState] = useState(initialState);
	const [error, setError] = useState("");

	const handleInput = (field) => (ev) =>
		setState({ ...state, [field]: ev.target.value });

	const isValid = state.username && state.email && state.password;

	const register = async () => {
		if (!isValid) return;

		const result = await fetch(`${API_SERVER}/user/register`, {
			method: "POST",
			body: JSON.stringify(state),
			headers: {
				"Content-Type": "application/json",
			},
		});

		if (result.body) {
			const data = await result.json();
			if (data.error) {
				setError(data.error);
			} else {
				alert("Account created - you may now log in.");
				window.location.reload();
			}
		}
	};

	return (
		<fieldset>
			<legend>Register new Account</legend>
			<form action="#" onSubmit={register}>
				<div
					style={{
						display: "flex",
						flexDirection: "column",
						gap: "0.5rem",
					}}
				>
					{error && <strong>{error}</strong>}
					<span>
						<label htmlFor="register-email">Email Address</label>
						<input
							type="email"
							required
							id="register-email"
							value={state.email}
							onInput={handleInput("email")}
						/>
					</span>
					<span>
						<label htmlFor="register-username">Username</label>
						<input
							type="text"
							autoComplete="username"
							required
							id="register-username"
							value={state.username}
							onInput={handleInput("username")}
						/>
					</span>
					<span>
						<label htmlFor="register-password">Password</label>
						<input
							type="password"
							autoComplete="current-password"
							required
							id="register-password"
							value={state.password}
							onInput={handleInput("password")}
						/>
					</span>
					<button disabled={!isValid} type="submit">
						Create Account
					</button>
				</div>
			</form>
		</fieldset>
	);
};
