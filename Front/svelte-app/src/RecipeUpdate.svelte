<script>
    import { onMount } from 'svelte';
    import { navigate } from 'svelte-routing';
    import { API_BASE_URL } from './constants.js';
    const accessToken = sessionStorage.getItem("accessToken"); 
    export let id;

    let recipe = null;
    let title = '';
    let description = '';
    let ingredients = '';
    let steps = '';
    let error = '';
    let successMessage = '';
    
    onMount(async () => {
      await loadRecipeData();
    });

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
        title = recipe.title;
        description = recipe.description;
      } catch (err) {
        error = err.message;
      }
    }
    // Handle form submission to create a new recipe
    async function updateRecipe() {
      error = ''; // Clear any previous errors

      // Validation
      if (description.trim().length < 5 || description.trim().length > 500) {
        error = 'Description must be between 5 and 500 characters.';
        return;
      }

      try {
        const response = await fetch(`${API_BASE_URL}/recipes/${id}`, {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${accessToken}`
          },
          body: JSON.stringify({
            description
          })
        });
  
        if (!response.ok) {
          error = await response.json();
          alert(error);
          throw new Error('Failed to update recipe');
        }
  
        const updatedRecipe = await response.json();
        successMessage = 'Recipe updated successfully!';
        // Redirect to the new recipe page
        navigate(`/recipes/${updatedRecipe.id}`);
      } catch (err) {
        error = err.message;
      }
    }
  </script>
  
  <div class="container mt-4">
    <h1>Update Recipe</h1>
    
    {#if error}
      <div class="alert alert-danger">
        {error}
      </div>
    {/if}
  
    {#if successMessage}
      <div class="alert alert-success">
        {successMessage}
      </div>
    {/if}
  
    <form on:submit|preventDefault={updateRecipe}>
      <div class="mb-3">
        <label for="title" class="form-label">Recipe Title</label>
        <input
          type="text"
          id="title"
          class="form-control readonly-field"
          bind:value={title}
          required
          readonly
        />
      </div>
  
      <div class="mb-3">
        <label for="description" class="form-label">Description</label>
        <textarea
          id="description"
          class="form-control {error && description.trim().length < 5 || description.trim().length > 500 ? 'is-invalid' : ''}"
          rows="3"
          bind:value={description}
          required
        ></textarea>
        {#if error && description.trim().length < 5 || description.trim().length > 500}
          <div class="invalid-feedback">
            Description must be between 5 and 500 characters.
          </div>
      {/if}
      </div>
  
      <button type="submit" class="btn btn-success">Update Recipe</button>
    </form>
  </div>
  
  <style>
    .container {
      max-width: 600px;
      margin: auto;
    }
  
    .form-label {
      font-weight: bold;
    }
  
    .form-control {
      margin-bottom: 1rem;
    }
  
    .btn {
      width: 100%;
    }

    .readonly-field {
    background-color: #f8f9fa; /* Light gray background */
    border: 1px solid #ccc;    /* Lighter border */
    color: #6c757d;            /* Gray text color */
    cursor: not-allowed;       /* Change cursor to indicate uneditable */
  }

  .readonly-field:focus {
    outline: none;  /* Remove focus outline */
  }
  </style>
  