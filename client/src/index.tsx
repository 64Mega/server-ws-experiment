import { createRoot } from "react-dom/client";
import { App } from "./App";

const container = document.getElementById("bloggy-root");

const appRoot = createRoot(container);

appRoot.render(<App />);
