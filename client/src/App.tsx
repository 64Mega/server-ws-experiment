import { BlogList } from "./features/blogList/BlogList";
import { CreateBlog } from "./features/blogList/CreateBlog";
import { Counter } from "./features/counter/Counter";
import { Login } from "./features/login/Login";
import { Register } from "./features/login/Register";
import { Notifier } from "./features/notifier/Notifier";
import { useAuth } from "./hooks/useAuth";

export const App = () => {
	const auth = useAuth();

	return (
		<div>
			<h2>Test App</h2>
			{auth.jwt ? (
				<>
					<button onClick={auth.logout}>Logout</button>
					<Counter />
					<Notifier />
					<div>
						<h3>Welcome!</h3>
						<div
							style={{
								display: "grid",
								gridTemplateColumns: "1fr 1fr",
							}}
						>
							<fieldset>
								<legend>Blog List</legend>
								<BlogList />
							</fieldset>
							<fieldset>
								<legend>Author a Blog</legend>
								<CreateBlog />
							</fieldset>
						</div>
					</div>
				</>
			) : (
				<>
					<Login />
					<Register />
				</>
			)}
		</div>
	);
};
