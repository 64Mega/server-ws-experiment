import { useState } from "react";

export const useAuth = () => {
	const storedJwt = localStorage.getItem('bloggy-jwt');
	const [jwt, setJwt] = useState(storedJwt);

	const updateJwt = (newJwt) => {
		localStorage.setItem('bloggy-jwt', newJwt);
		setJwt(newJwt);
	}

	const logout = () => {
		localStorage.removeItem('bloggy-jwt');
		setJwt('');
		window.location.reload();
	}

	return { jwt, updateJwt, logout };
}