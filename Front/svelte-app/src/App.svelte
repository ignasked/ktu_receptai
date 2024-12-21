<script>
	import "bootstrap/dist/css/bootstrap.min.css";
	import './global.css';
	//import '/fonts/bootstrap-icons.css';
	//import 'https://cdn.jsdelivr.net/npm/bootstrap-icons/font/bootstrap-icons.css';
	//import './assets/css/bootstrap-icons.css';
	import { API_BASE_URL } from './constants.js';
	import { Router, Route, Link, navigate} from "svelte-routing";
	import { isLoggedIn, getUserName, isAuthenticated, userName } from "./jwtHelpers";
	import Home from "./Home.svelte";
	import Login from './Login.svelte';
	import Register from "./Register.svelte";
	import RecipeList from "./RecipeList.svelte";
	import RecipeView from "./RecipeView.svelte";
	import RecipeCook from "./RecipeCook.svelte";
	import RecipeCreate from "./RecipeCreate.svelte";
	import RecipeUpdate from "./RecipeUpdate.svelte";

	import { onMount } from 'svelte';
    onMount(() => {
        isLoggedIn();
    });

	async function logout() {
		try {
			const accessToken = sessionStorage.getItem("accessToken");
			// Call your API to log the user out
			const response = await fetch(`${API_BASE_URL}/logout`, {
			method: 'POST',
			headers: {
				'Authorization': `Bearer ${accessToken}`
			}
			});

			// Check if the response is not OK (status code >= 400)
			if (!response.ok) {
				// Try to parse the error message from the response
				const errorDetails = await response.json().catch(() => null); // Safely parse JSON
				const errorMessage = errorDetails?.message || `Error ${response.status}: ${response.statusText}`;
				throw new Error(errorMessage); // Throw detailed error
        	}
			sessionStorage.setItem("accessToken", "");
			isAuthenticated.set(false); // Update store
            userName.set(''); // Clear user name
			// Optionally, clear cookies if using cookies for authentication
			//document.cookie = 'authToken=; Max-Age=0'; // Clear the authToken cookie if used

			// Redirect to the login page
			navigate('/login');
		} catch (err) {
			console.error('Logout failed:', err);
		}
	}
</script>


<Router>
	<nav class="navbar navbar-expand-lg navbar-light bg-light">
		<div class="container-fluid">
			<!-- Brand -->
			<Link class="navbar-brand" to="/">Recipe Master</Link>

			<!-- Hamburger button for mobile -->
			<button
				class="navbar-toggler"
				type="button"
				data-bs-toggle="collapse"
				data-bs-target="#navbarNav"
				aria-controls="navbarNav"
				aria-expanded="false"
				aria-label="Toggle navigation"
			>
				<span class="navbar-toggler-icon"></span>
			</button>

			<!-- Navigation links -->
			<div class="collapse navbar-collapse" id="navbarNav">
				<ul class="navbar-nav ms-auto">
					<li class="nav-item">
						<Link class="nav-link" to="/">Home</Link>
					</li>
					<li class="nav-item">
						<Link class="nav-link" to="/recipes">Recipes</Link>
					</li>
					{#if $isAuthenticated === false}
						<li class="nav-item">
							<Link class="nav-link" to="/register">Register</Link>
						</li>
						<li class="nav-item">
							<Link class="nav-link" to="/login">Login</Link>
						</li>
					{/if}
					{#if $isAuthenticated === true}    				
						<li class="nav-item">
							<button class="btn btn-link nav-link" on:click={logout}>Logout</button>
							
						</li>
						<li class="nav-item">
							<p class ="nav-link">{getUserName()}</p>	
						</li>
						
					{/if}
				</ul>
			</div>
		</div>
	</nav>

	<Route path="/" component={Home} />
	<Route path="*" component={Home} />
	<Route path="/login" component={Login} />
	<Route path="/register" component={Register} />
	<Route path="/recipes" component={RecipeList} />
	<Route path="/recipes/:id" component={RecipeView} />
	<Route path="/recipes/:id/cook/" component={RecipeCook} />
	<Route path="/recipe/create/" component={RecipeCreate} />
	<Route path="/recipe/update/:id" component={RecipeUpdate} />

</Router>
<footer class="text-center py-3 bg-light">
	<p>&copy; {new Date().getFullYear()} Recipe Master. All Rights Reserved.</p>
</footer>

<style>
	/* Add custom styles here */
	.navbar-brand {
		font-weight: bold;
	}
	.nav-link {
		cursor: pointer;
	}
	@media (max-width: 768px) {
		.navbar-nav {
			text-align: center;
		}
	}
</style>