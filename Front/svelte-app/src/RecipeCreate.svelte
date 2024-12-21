<script>
    import { navigate } from 'svelte-routing';
    import { API_BASE_URL } from './constants.js';
    const accessToken = sessionStorage.getItem("accessToken");

    let title = '';
    let description = '';
    let ingredients = '';
    let steps = '';
    let error = '';
    let successMessage = '';
    
  
    // Handle form submission to create a new recipe
    async function createRecipe() {
      error = ''; // Clear any previous errors

      // Validation
      if (title.trim().length < 2 || title.trim().length > 100) {
        error = 'Title must be between 2 and 100 characters.';
        return;
      }
      if (description.trim().length < 5 || description.trim().length > 500) {
        error = 'Description must be between 5 and 500 characters.';
        return;
      }

      try {
        const response = await fetch(`${API_BASE_URL}/recipes`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${accessToken}`
          },
          body: JSON.stringify({
            title,
            description
          })
        });
  
        if (!response.ok) {
          error = await response.json();
          alert(error);
          throw new Error('Failed to create recipe');
        }
  
        const createdRecipe = await response.json();
        successMessage = 'Recipe created successfully!';
        // Redirect to the new recipe page
        navigate(`/recipes/${createdRecipe.id}`);
      } catch (err) {
        error = err.message;
      }
    }
  </script>
  
  <div class="container mt-4">
    <h1>Create New Recipe</h1>
    
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
  
    <form on:submit|preventDefault={createRecipe}>
      <div class="mb-3">
        <label for="title" class="form-label">Recipe Title</label>
        <input
          type="text"
          id="title"
          class="form-control {error && title.trim().length < 2 || title.trim().length > 100 ? 'is-invalid' : ''}"
          bind:value={title}
          required
        />
        {#if error && title.trim().length < 2 || title.trim().length > 100}
        <div class="invalid-feedback">
          Title must be between 2 and 100 characters.
        </div>
      {/if}
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
  
      <button type="submit" class="btn btn-success">Create Recipe</button>
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
  </style>
  