<script>
	import { API_BASE_URL } from './constants.js';
	import { navigate } from 'svelte-routing';

	// You can add props or fetch dynamic data here
	let featuredRecipes = [
		{ title: "Classic Spaghetti", image: "/images/spaghetti.jpg", description: "A timeless classic with a rich tomato sauce.", id: 1 },
		{ title: "Healthy Salad Bowl", image: "/images/salad.jpg", description: "Fresh veggies with a zesty dressing.", id: 2 },
		{ title: "Chocolate Lava Cake", image: "/images/lava_cake.jpg", description: "Indulgent dessert with a molten chocolate center.", id: 3 }
	];

	async function loadAllRecipes(){
    try {
      const response = await fetch(`${API_BASE_URL}/recipes`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      });
      if (!response.ok) {
        error = await response.json();
        alert(error);
        throw new Error('Failed to fetch recipes');
      }
      recipes = await response.json();
	  featuredRecipes = recipes.slice(0, 3);
    } catch (err) {
      error = err.message;
    }
  }
</script>



<div class="hero-section text-center text-dark py-5">
	<h1>Welcome to My Recipes!</h1>
	<p>Your ultimate destination for cooking inspiration.</p>
	<button	on:click={() => navigate('/recipes')} class="btn btn-primary btn-lg mt-3">Explore Recipes</button>
</div>

<div class="container my-5">
	<!-- Featured Recipes -->
	<h2 class="text-center mb-4">Featured Recipes</h2>
	<div class="row">
		{#each featuredRecipes as recipe}
			<div class="col-md-4 mb-4">
				<div class="card">
					<img src={"/default-recipe-image.jpg"} class="card-img-top" alt={recipe.title} />
					<div class="card-body">
						<h5 class="card-title">{recipe.title}</h5>
						<p class="card-text">{recipe.description}</p>
						<button on:click={() => navigate(`/recipes/${recipe.id}`)} class="btn btn-secondary">View Recipe</button>
					</div>
				</div>
			</div>
		{/each}
	</div>
</div>

<div class="cta-section text-center text-light py-5 bg-secondary">
	<h2>Join Our Cooking Community</h2>
	<p>Sign up to save your favorite recipes, share your own, and connect with other cooking enthusiasts.</p>
	<button on:click={() => navigate('/register')} class="btn btn-warning btn-lg mt-3">Sign Up Now</button>
</div>

<style>
	.hero-section {
		background-image: url('/hero-bg2.jpg');
		background-size: cover;
		background-position: center;
		color: #ffffff;
		min-height: 60vh;
		display: flex;
		flex-direction: column;
		justify-content: center;
		align-items: center;
	}
	.card img {
		height: 200px;
		object-fit: cover;
	}
	.cta-section {
		background-color: #6c757d;
	}
	footer {
		font-size: 0.9rem;
		color: #333;
	}
</style>
  