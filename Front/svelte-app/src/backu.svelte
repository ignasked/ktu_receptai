<script>
    import { onMount } from 'svelte';
    import { API_BASE_URL } from './constants.js';
    import { navigate } from 'svelte-routing';
    import { Star } from 'svelte-bootstrap-icons'; // Import the Star icon from the library
    import { checkAuthFunctions, getAccessToken } from './jwtHelpers.js';
    export let id;
    let stepId=-1;
    let ingredientId=-1;
  
    let recipe = null;
    let error = '';
    let steps = [];
    let stepIngredients = [];
    let minStepDuration = 6;//in seconds

    let addingStep = false; // Toggle form visibility
    let editingStep = false;
    let editingIngredient = false;
    let addingIngredient = false;

    let newStep = { text: '', duration: minStepDuration}; // New step data
    let newIngredient = { name: '', amount: 0, unit: '' };
    
    onMount(   
    async () => {
      await Promise.all([loadRecipeData(), loadRecipeSteps()]);
    });

    // Toggle Modal Visibility
    function toggleAddStepModal() {
      addingStep = !addingStep;

      // Add/remove blurred class when the modal is toggled
      if (addingStep) {
        document.body.classList.add('blurred');
        newStep.text = '';
        newStep.duration = minStepDuration;
      } else {
        document.body.classList.remove('blurred');
      }
      error = '';
    }
    function toggleEditStepModal() {
      editingStep = !editingStep;

      // Add/remove blurred class when the modal is toggled
      if (editingStep) {
        document.body.classList.add('blurred');
      } else {
        document.body.classList.remove('blurred');
      }
      error = '';
    }
    

    //Loading
    async function loadRecipeData(){
      try {
          const response = await fetch(`${API_BASE_URL}/recipes/${id}`, {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
          },
          credentials: 'include',
        });
        if (!response.ok) {
          error = await response.json();
          alert(error);
          throw new Error('Recipe not found');
        }
        recipe = await response.json();
      } catch (err) {
        error = err.message;
      }
    }
    async function loadRecipeSteps(){
      try {
        const response = await fetch(`${API_BASE_URL}/recipes/${id}/steps`);
        if (!response.ok) {
          error = await response.json();
          alert(error);
          throw new Error('Recipe not found');
        }
        steps = await response.json();
      } catch (err) {
        error = err.message;  
      }
    }
    async function loadStepIngredientsData() {
    try {
      const response = await fetch(`${API_BASE_URL}/recipes/${id}/steps/${stepId}/ingredients`, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json'},
      });

      if (!response.ok) {
        error = await response.json();
        alert(error);
        throw new Error('Failed to load steps indexes');
      }

      stepIngredients = await response.json();      
    } catch (err) {
      error = err.message;
    }
    }
  
    //Deletion
    async function deleteRecipe() {
      try {
        const response = await fetch(`${API_BASE_URL}/recipes/${id}`, {
          method: 'DELETE',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${getAccessToken()}`
          },
        });
        // Check if the response is not OK (status code >= 400)
        if (!response.ok) {

            error = await response.json();
            alert(error);
            throw new Error(errorMessage); // Throw detailed error
        }
        console.log('Recipe deleted successfully!');
        navigate('/recipes');
      } catch (err) {
        console.error('Error deleting recipe:', err);
        error = err.message;
      }
    }  
    async function deleteStep(stepId) {
      console.log(`Delete step ${stepId} in recipe ${id}`);
      try {
        const response = await fetch(`${API_BASE_URL}/recipes/${id}/steps/${stepId}`, {
          method: 'DELETE',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${getAccessToken()}`
          },
        });
        // Check if the response is not OK (status code >= 400)
        if (!response.ok) {
          error = await response.json();
          alert(error);
          throw new Error(errorMessage); // Throw detailed error
        }
        console.log('step deleted successfully!');
        loadRecipeSteps();
        //navigate('/recipes');
      } catch (err) {
        console.error('Error deleting recipe:', err);
        error = err.message;
      }
    }  
    async function deleteIngredient(delIngredientId) {
      console.log(`Delete ingredient ${delIngredientId} in step ${stepId} in recipe ${id}`);
      try {
        const response = await fetch(`${API_BASE_URL}/recipes/${id}/steps/${stepId}/ingredients/${delIngredientId}`, {
          method: 'DELETE',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${getAccessToken()}`
          },
        });
        // Check if the response is not OK (status code >= 400)
        if (!response.ok) {
          error = await response.json();
          alert(error);
          throw new Error(errorMessage); // Throw detailed error
        }
        console.log('ingredient deleted successfully!');
        loadStepIngredientsData();
        //navigate('/recipes');
      } catch (err) {
        console.error('Error deleting recipe:', err);
        error = err.message;
      }
    } 

    //Adding
    async function addNewStep() {
      error = '';
      //Validation
      if (newStep.duration < minStepDuration) {
        error = "Step duration must be at least " + minStepDuration + " seconds.";
        alert('Step duration must be at least ' + minStepDuration + ' seconds.');
        return;
      }
      if(newStep.text.trim().length >500 || newStep.text.trim().length < 2){
        error = "Step description must be between 2 and 500 characters.";
        alert('Step description must be between 2 and 500 characters.');
        return;
      }

      newStep.duration = formatDurationToDateTime(newStep.duration);

      
      

      console.log(newStep);
        if (!newStep.text.trim()) {
            alert('Step description cannot be empty.');
            return;
        }
        try {
            const response = await fetch(`${API_BASE_URL}/recipes/${id}/steps`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json',
                            'Authorization': `Bearer ${sessionStorage.getItem("accessToken")}`
                 },
                body: JSON.stringify(newStep),
            });

            if (!response.ok){
              error = await response.json();
              alert(error);
              throw new Error('Failed to add step.');
            } 

            const addedStep = await response.json();
            steps = [...steps, addedStep]; // Add the new step to the list
            newStep = { text: '', duration: minStepDuration.toString()}; // Reset the form
            addingStep = false; // Hide the form
        } catch (err) {
            error = err.message;
        }
    }
    async function addNewIngredient() {     
      console.log(newIngredient);
        if (!newIngredient.name.trim()) {
            alert('Step description cannot be empty.');
            return;
        }
        try {
            const response = await fetch(`${API_BASE_URL}/recipes/${id}/steps/${stepId}/ingredients`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json',
                            'Authorization': `Bearer ${sessionStorage.getItem("accessToken")}`
                 },
                body: JSON.stringify(newIngredient),
            });

            if (!response.ok){
              error = await response.json();
              alert(error);
              throw new Error('Failed to add ingredient.');
            } 

            const addedIngredient = await response.json();
            stepIngredients = [...stepIngredients, addedIngredient]; // Add the new step to the list
            //newStep = { text: '', duration: minStepDuration.toString()}; // Reset the form
            newIngredient = { name: '', amount: 0, unit: '' };
            //addingStep = false; // Hide the form
        } catch (err) {
            error = err.message;
        }
    }

    //Editing
    async function updateStep() {
      newStep.duration = formatDurationToDateTime(newStep.duration);
      console.log(newStep);
        if (!newStep.text.trim()) {
            alert('Step description cannot be empty.');
            return;
        }
        try {
            const response = await fetch(`${API_BASE_URL}/recipes/${id}/steps/${stepId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json',
                            'Authorization': `Bearer ${sessionStorage.getItem("accessToken")}`
                 },
                body: JSON.stringify(newStep),
            });

            if (!response.ok){
              error = await response.json();
              alert(error);
              throw new Error('Failed to add step.');
            } 

            const addedStep = await response.json();
            steps = await loadRecipeSteps(); // Add the new step to the list
            newStep = { text: '', duration: minStepDuration.toString()}; // Reset the form
            editingStep = false;// Hide the form
        } catch (err) {
            error = err.message;
        }
    }
    async function updateIngredient() {
      //newStep.duration = formatDurationToDateTime(newStep.duration);
      console.log(newIngredient);
        if (!newIngredient.name.trim()) {
            alert('Step description cannot be empty.');
            return;
        }
        try {
            const response = await fetch(`${API_BASE_URL}/recipes/${id}/steps/${stepId}/ingredients/${ingredientId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json',
                            'Authorization': `Bearer ${sessionStorage.getItem("accessToken")}`
                 },
                body: JSON.stringify(newIngredient),
            });

            if (!response.ok){
              error = await response.json();
              alert(error);
              throw new Error('Failed to add step.');
            } 

            const addedIngredient = await response.json();
            stepIngredients = await loadStepIngredientsData(); // Add the new step to the list
            newIngredient = { name: '', amount: 0, unit: '' }; // Reset the form\
            editingIngredient = false;// Hide the form
            //editingStep = false;// Hide the form
        } catch (err) {
            error = err.message;
        }
    }

    function startEditingStep(step) {
      stepId = step.id;
      newStep.text = step.text;
      newStep.duration = formatSecondsFromDateTime(step.duration);
      loadStepIngredientsData();
      console.log(newStep);
      toggleEditStepModal();
    }
    function startEditingIngredient(ingredient) {
      editingIngredient = true;
      ingredientId = ingredient.id;
      newIngredient.name = ingredient.name;
      newIngredient.amount = ingredient.amount;
      newIngredient.unit = ingredient.unit;
      console.log(newIngredient);
    }
    function startAddingIngredient() {
      addingIngredient = true;
      newIngredient.name = '';
      newIngredient.amount = 0;
      newIngredient.unit = '';   
    }

    // Helper function to format duration (e.g., "00:10:00" -> "10 minutes")
  function formatDurationFromDateTime(duration) {
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
  function formatDurationFromSeconds(seconds) {
        const hours = Math.floor(seconds / 3600);
        const minutes = Math.floor((seconds % 3600) / 60);
        const secs = seconds % 60;
        return `${hours > 0 ? hours + 'h ' : ''}${minutes}m ${secs}s`;
  }
  // Format total seconds into "HH:MM:SS"
  function formatDurationToDateTime(seconds) {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const remainingSeconds = seconds % 60;

    const paddedHours = String(hours).padStart(2, '0');
    const paddedMinutes = String(minutes).padStart(2, '0');
    const paddedSeconds = String(remainingSeconds).padStart(2, '0');
    
    return `${paddedHours}:${paddedMinutes}:${paddedSeconds}`;
  }
  function formatSecondsFromDateTime(duration) {
    const parts = duration.split(':');
    const hours = parseInt(parts[0]);
    const minutes = parseInt(parts[1]);
    const seconds = parseInt(parts[2]);
    return hours * 3600 + minutes * 60 + seconds;   
  }
  </script>

<!-- Add Step Modal -->
{#if addingStep || editingStep}
  <div class="modal" tabindex="-1" style="display:block;" role="dialog">
    <div class="modal-dialog" role="document">
      <div class="modal-content">
        <div class="modal-header">
          {#if addingStep}
            <h5 class="modal-title">Add New Step</h5>
          {/if}
          {#if editingStep}
            <h5 class="modal-title">Edit Step</h5>
          {/if}
        </div>
        <div class="modal-body">
          {#if error}
            <div class="alert alert-danger">{error}</div>
          {/if}
          <div class="form-group">
            <label for="newStep.text">Step Description</label>
            <textarea class="form-control" id="newStep.text" bind:value={newStep.text}></textarea>
          </div>
          <div class="form-group">
            <label for="stepDuration">Duration</label>
            <input
              type="range"
              class="form-control"
              id="stepDuration"
              min={minStepDuration}
              max="3600"
              bind:value={newStep.duration}
            />
            <p>{formatDurationFromSeconds(newStep.duration)}</p>
          </div>
        
        {#if editingStep}        
          <!-- Ingredients List -->
          {#if stepIngredients != null && stepIngredients.length > 0}
            <h5 class="modal-title">Ingredient list:</h5>
            <ul>
              {#each stepIngredients as ingredient}
                <li>
                  <button class="btn btn-info btn-sm" on:click={() => startEditingIngredient(ingredient)}>Edit</button>
                  <button class="btn btn-danger btn-sm" on:click={() => deleteIngredient(ingredient.id)}>Delete</button>
                  {ingredient.name}( {ingredient.amount} {ingredient.unit})
                </li> <!-- Assuming each ingredient is a simple text; adjust if it's more complex -->
              {/each}
            </ul>
          {/if}
          {#if !addingIngredient}
            <button class="btn btn-success btn-sm" on:click={() => startAddingIngredient()}>Add Ingredient</button>
          {/if}
          

          {#if addingIngredient || editingIngredient}
          <!-- Ingredients -->
            <div class="d-flex flex-column mt-3">
              <div class="mb-3">
                <label for="newIngredient.name">Ingredient</label>
                <input type="text" class="form-control" id="newIngredient.name" bind:value={newIngredient.name} placeholder="enter ingredient name">
              </div>
              <div class="mb-3">
                <label for="field2">Amount</label>
                <input type="text" class="form-control" id="field2" bind:value={newIngredient.amount} placeholder="enter amount">
              </div>
              <div class="mb-3">
                <label for="field3">Unit</label>
                <input type="text" class="form-control" id="field3" bind:value={newIngredient.unit} placeholder="enter unit">
              </div>
            </div>
            {#if !editingIngredient}
              <div class="form-group mt-3">
                <button type="button" class="btn btn-success" on:click={addNewIngredient}>Add Ingredient</button>
              </div>
            {/if}
            {#if editingIngredient}
              <div class="form-group mt-3">
                <button type="button" class="btn btn-success" on:click={updateIngredient}>Update Ingredient</button>
              </div>
            {/if}
          {/if}
        {/if}
      </div>
        <div class="modal-footer">
          
          {#if editingStep}
            <button type="button" class="btn btn-secondary" on:click={toggleEditStepModal}>Cancel</button>
            <button type="button" class="btn btn-primary" on:click={updateStep}>Save</button>
          {/if}
          {#if addingStep}
            <button type="button" class="btn btn-secondary" on:click={toggleAddStepModal}>Cancel</button>
            <button type="button" class="btn btn-primary" on:click={addNewStep}>Add</button>
          {/if}
        </div>
      </div>
    </div>
  </div>
{/if}
<!-- Modal Backdrop -->
{#if addingStep || editingStep}
  <div class="modal-backdrop fade show"></div>
{/if}

<!-- Page Layout -->
<div class="container mt-4">
  {#if error}
    <div class="alert alert-danger">{error}</div>
  {:else if recipe}
    <div class="row">
      <!-- Back Button -->
      <div class="col-md-2 mb-4">
        <button class="btn btn-secondary" on:click={() => navigate('/recipes')}>Back to All Recipes</button>
      </div>

      <!-- Recipe Image -->
      <div class="col-md-6 mb-4">
        <img
          src={'/default-recipe-image.jpg'}
          alt={recipe.title}
          class="img-fluid rounded shadow"
        />
      </div>

      <!-- Recipe Info -->
      <div class="col-md-6 mb-4">
        <h1>{recipe.title}</h1>
        <!-- Rating Display -->
        <div class="rating">
          {#each Array(5) as _, index}
            <Star
              class="star"
              style="color: {index < recipe.rating ? '#ffd700' : '#ccc'}; font-size: 50px;"
            />
          {/each}
        </div>
        <p>{recipe.description}</p>
        <div class="col-md-4 mb-4">
          <button class="btn btn-primary" on:click={navigate(`/recipes/${id}/cook`)}>Start this recipe</button>
          {#if checkAuthFunctions(recipe.userId)}
            <button class="btn btn-secondary" on:click={navigate(`/recipe/update/${id}`)}>Edit Recipe</button>
            <button class="btn btn-danger" on:click={deleteRecipe}>Delete Recipe</button>
          {/if}
        </div>
        <div class="col-md-2 mb-4">
          
        </div>
      </div>
    </div>

    <!-- Steps Section -->

    
    <!-- Header with Add Step Button -->
    <div class="header d-flex align-items-center mb-4">
      <h1 class="me-3">Steps</h1> <!-- Added margin to separate the button -->
      {#if checkAuthFunctions(recipe.userId)}
        <button
          class="btn btn-success btn-lg rounded-circle add-btn"
          on:click={toggleAddStepModal}
          title="Add New Step"
        >
          +
        </button>
      {/if}
    </div>
   
    <ol>
      {#each steps as step, index}
        <li>
          <p>{step.text}</p>
          <p><small>Duration: {formatDurationFromDateTime(step.duration)}</small></p>
          <button on:click={deleteStep(step.id)}>Delete</button>
          <button on:click={startEditingStep(step)}>Edit</button>
        </li>
      {/each}
    </ol>
  {/if}
</div>




  
<style>
  /* Ensure the image is responsive and looks good on all screen sizes */
  .img-fluid {
    width: 100%;
    height: auto;
    object-fit: cover;
  }

  /* Card-style effect for the image */
  .shadow {
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  }

  /* Make the heading more prominent */
  h1 {
    font-size: 2rem;
    font-weight: 600;
  }

  /* Add some margin to the description for better spacing */
  p {
    font-size: 1.1rem;
    line-height: 1.6;
    color: #555;
  }

  /* Add spacing and style the button */
  button {
    margin-top: 20px;
  }

  /* Rating stars styling */
  .rating {
    display: flex;
  }

  /* Style the individual stars if needed */
  .star {
    margin-right: 5px;
  }

  /* Add responsive design for mobile devices */
  @media (max-width: 768px) {
    .col-md-6 {
      margin-bottom: 20px;
    }
    h1 {
      font-size: 1.6rem;
    }
    p {
      font-size: 1rem;
    }
      /* Make the stars responsive */
    .rating .star {
      font-size: 20px; /* smaller on mobile */
    }
  }
    /* Increase the size for larger screens */
  @media (min-width: 768px) {
    .rating .star {
      font-size: 30px; /* standard size on larger screens */
    }
  }

  /* Styling adjustments for steps */
  ol {
    margin-top: 20px;
    padding-left: 20px;
  }

  li {
    margin-bottom: 15px;
    line-height: 1.5;
  }

  p {
    margin: 0;
  }

  small {
    color: #555;
  }

  .header {
      display: flex;
      align-items: center;
    }

    .me-3 {
      margin-right: 1rem; /* Adjust spacing between the heading and button */
    }

    /* Modals */

   /* Blur effect for the background */
  .blurred {
    filter: blur(100px); /* Adjust the blur level as needed */
    pointer-events: none; /* Prevent interaction with the blurred background */
  }

  /* Ensure the modal is above the blurred background */
  .modal, .modal-dialog, .modal-content {
    position: relative;
    z-index: 1050; /* Make sure modal content appears above the blurred content */
    display: flex;
    flex-direction: column;
  }

  /* Optionally, you can adjust the opacity of the blurred background */
  .blurred * {
    pointer-events: none;
  }

  /* Add some basic styling for the modal backdrop */
  .modal-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.5);
    z-index: 1040; /* Make sure the backdrop appears below the modal */
  }

  .modal {
    position: fixed;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    z-index: 1050; /* Ensure the modal appears above the blurred background */
  }

  /* Adjust modal close button and other modal styles */
  .close {
    font-size: 1.5rem;
    color: #000;
  }
</style>