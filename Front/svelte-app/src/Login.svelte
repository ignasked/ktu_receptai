<script>
  import { onMount } from 'svelte';
  import { API_BASE_URL } from './constants.js';
  import { navigate } from 'svelte-routing';
  //import '/fonts/bootstrap-icons.css';
  //import 'https://cdn.jsdelivr.net/npm/bootstrap-icons/font/bootstrap-icons.css';
  //import './assets/css/bootstrap-icons.css';
  import {Lock, Person} from 'svelte-bootstrap-icons';
  import {getUserName, isLoggedIn} from './jwtHelpers.js';



  let username = '';
  let password = '';
  let error = '';
  
  const loginUrl = `${API_BASE_URL}/login`;
  async function handleLogin(event) {
    event.preventDefault(); // Prevent form submission refresh
    error = ''; // Reset errors

    try {
      const response = await fetch(loginUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password }),
      });

      if (!response.ok) {
        const errorData = await response.json();
        error = errorData || 'Login failed!';
        alert(error);
      } else {
        const data = await response.json();
        console.log('Login successful!', data.accessToken); // Handle success (e.g., save token)
        sessionStorage.setItem('accessToken', data.accessToken);
        getUserName();
        isLoggedIn();
        navigate('/recipes');
      }
    } catch (err) {
      error = 'An error occurred. Please try again.';
      console.error(err);
    }
  }
</script>

<form on:submit={handleLogin} class="form-signin w-100 p-4">
  <h1 class="h3 mb-3 font-weight-normal text-center">Login</h1>

  <div class="form-group">
    <div class="input-group">
      <div class="input-group-prepend">
        <span class="input-group-text">
          <Person />  <!-- Person Icon -->
        </span>
      </div>
      <input
        type="text"
        id="username"
        class="form-control"
        bind:value={username}
        placeholder="Enter your username"
        required
      />
    </div>
  </div>


<div>
  
</div>
  <!-- Password -->
  <div class="form-group mt-2">
    <div class="input-group">
      <div class="input-group-prepend">
        <span class="input-group-text">
          <Lock/>  <!-- Lock Icon -->
        </span>
      </div>
      <input
        type="password"
        id="password"
        class="form-control"
        bind:value={password}
        placeholder="Enter your password"
        required
      />
    </div>
  </div>

  <!-- Error Message -->
  {#if error}
    <div class="alert alert-danger">
      {error}
    </div>
  {/if}

  <!-- Submit Button -->
  <button type="submit" class="btn btn-primary w-100 mt-4">Login</button>
  <label class="text-center mt-4">Don't have an account?</label>
  <button type="button" class="btn btn-primary w-100" on:click={() => navigate('/register')}>Register</button>
</form>

<style>
  form {
    max-width: 400px;
    margin: auto;
  }
  .error {
    color: red;
  }
  .input-group-text {
    padding: 0.6rem; /* Adjust padding */
  }

  .input-group-text svg {
    height: 4em; /* Match to input font size */
    width: 4em;
  }
</style>