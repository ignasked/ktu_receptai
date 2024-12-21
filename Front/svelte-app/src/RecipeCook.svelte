<script>
  import { onMount } from 'svelte';
  import { API_BASE_URL } from './constants.js';
  import {navigate} from 'svelte-routing';

  export let id; // Recipe ID passed to the component

  let recipeTitle = '';
  let currentStep = null;
  let stepIds = []; // Array of all step IDs
  let stepIngredients = [];
  let currentStepIndex = 0; // Index of the current step in `stepIds`
  let error = '';

  // Fetch the recipe title
  async function loadRecipeTitle() {
    try {
      const response = await fetch(`${API_BASE_URL}/recipes/${id}`, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
      });

      if (!response.ok) {
        error = await response.json();
        alert(error);
        throw new Error('Failed to load recipe title');
      }

      const recipe = await response.json();
      recipeTitle = recipe.title || 'Recipe'; // Assuming all steps belong to the same recipe
    } catch (err) {
      error = err.message;
    }
  }

  // Fetch step IDs upfront
  async function loadAllStepsData() {
    try {
      const response = await fetch(`${API_BASE_URL}/recipes/${id}/steps`, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
      });

      if (!response.ok) {
        error = await response.json();
        alert(error);
        throw new Error('Failed to load steps indexes');
      }

      const steps = await response.json();
      stepIds = steps.map((step) => step.id); // Extract step IDs from the response

      if (stepIds.length > 0) {
        await loadStepData(0); // Load the first step
      }
    } catch (err) {
      error = err.message;
    }
  }

  async function loadStepIngredientsData() {
    console.log("Loading step ingredients");
    try {
      const response = await fetch(`${API_BASE_URL}/recipes/${id}/steps/${currentStep.id}/ingredients`, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json'},
      });

      if (!response.ok) {
        error = await response.json();
        alert(error);
        throw new Error('Failed to load steps indexes');
      }

      stepIngredients = await response.json();
      console.log(stepIngredients);
      //stepIds = steps.map((step) => step.id); // Extract step IDs from the response

      /*if (stepIds.length > 0) {
        await loadStepData(0); // Load the first step
      }*/
    } catch (err) {
      error = err.message;
    }
  }


  // Fetch the data for the step at `stepIndex`
  async function loadStepData(stepIndex) {
    try {
      const stepId = stepIds[stepIndex];
      const response = await fetch(`${API_BASE_URL}/recipes/${id}/steps/${stepId}`, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
      });

      if (!response.ok) {
        error = await response.json();
        alert(error);
        throw new Error('Failed to load a step');
      }

      currentStep = await response.json();
      currentStepIndex = stepIndex;

      await loadStepIngredientsData();
    } catch (err) {
      error = err.message;
    }
  }

  // Navigate to the previous step
  async function previousStep() {
    if (currentStepIndex > 0) {
      await loadStepData(currentStepIndex - 1);
    }
  }

  // Navigate to the next step
  async function nextStep() {
    if (currentStepIndex < stepIds.length - 1) {
      await loadStepData(currentStepIndex + 1);
    }
  }

  onMount(async () => {
    try {
      // Load recipe data and steps in parallel
      await Promise.all([loadRecipeTitle(), loadAllStepsData()]);
    } catch (err) {
      error = err.message;
    }
  });

  // Helper function to format duration (e.g., "00:10:00" -> "10 minutes")
  function formatDuration(duration) {
    const parts = duration.split(':');
    const hours = parseInt(parts[0]);
    const minutes = parseInt(parts[1]);
    const seconds = parseInt(parts[2]);
    let formatted = '';
    if (hours > 0) formatted += `${hours} hours `;
    if (minutes > 0) formatted += `${minutes} minutes `;
    if (seconds > 0) formatted += `${seconds} seconds`;
    return formatted.trim();
  }
</script>

<!-- Page Layout -->
<div class="container mt-4">
  {#if error}
    <div class="alert alert-danger">{error}</div>
  {:else if recipeTitle && currentStep}
    <div class="row">
      <!-- Recipe Title -->
      <div class="col-md-12 text-center">
        <h1>{recipeTitle}</h1>
        <button class="btn btn-secondary"on:click={navigate(`/recipes/${id}`)}> Back to Recipe Page</button>
      </div>
    </div>

    <!-- Step Display -->
    <div class="row mt-4">
      <div class="col-md-12 text-center">
        <h2>Step {currentStepIndex + 1} of {stepIds.length}</h2>
        <p>{currentStep.text}</p>
        <p><strong>Duration:</strong> {formatDuration(currentStep.duration)}</p>
        <!-- Display Ingredients -->
        <h3>Ingredients: </h3>
        {#if stepIngredients.length > 0}
          <ul>
            {#each stepIngredients as ingredient}
              <li>{ingredient.name}( {ingredient.amount} {ingredient.unit})</li> <!-- Assuming each ingredient is a simple text; adjust if it's more complex -->
            {/each}
          </ul>
        {/if}
      </div>
    </div>

    <!-- Navigation Buttons -->
    <div class="row mt-4">
      <div class="col-md-6 text-center">
        <button
          class="btn btn-primary"
          on:click={previousStep}
          disabled={currentStepIndex === 0}
        >
          Previous Step
        </button>
      </div>
      <div class="col-md-6 text-center">
        <button
          class="btn btn-primary"
          on:click={nextStep}
          disabled={currentStepIndex === stepIds.length - 1}
        >
          Next Step
        </button>
      </div>
    </div>
  {/if}
</div>

<style>
  h1 {
    font-size: 2.5rem;
    font-weight: bold;
    margin-bottom: 20px;
  }

  h2 {
    font-size: 1.5rem;
    margin-bottom: 10px;
  }

  p {
    font-size: 1.2rem;
    margin: 10px 0;
  }

  button {
    font-size: 1.2rem;
    padding: 10px 20px;
  }

  button:disabled {
    background-color: #ccc;
    border: none;
    cursor: not-allowed;
  }

  .container {
    max-width: 800px;
    margin: 0 auto;
    padding: 20px;
  }
</style>
