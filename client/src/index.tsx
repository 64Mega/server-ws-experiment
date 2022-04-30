import { createRoot } from "react-dom/client";

const container = document.getElementById("bloggy-root");

const appRoot = createRoot(container);

appRoot.render(<h1>Hello World</h1>);
