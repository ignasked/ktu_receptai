<script>
    import { onMount } from "svelte";
    import { API_BASE_URL } from './constants.js';
    import { navigate } from "svelte-routing";
    let error = '';

    let Username = "";
    let Email = "";
    let Password = "";

    const registerUrl = `${API_BASE_URL}/accounts`;

    const handleRegister = async () => {
        error = ''; // Clear any previous errors

        // Validation
        // Validation
        const passwordRegex = /^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{16,100}$/;
        if (!passwordRegex.test(Password)) {
            error = 'Password must be 16-100 characters long, with at least one uppercase letter, one number, and one special character.';
            return;
        }

        try {
            const response = await fetch(registerUrl, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ Username, Password, Email }),
            });

            if (response.ok) {
                alert("Registration successful!");
                // Redirect or handle success
                navigate("/login");
            } else {
                const error1 = await response.json();
                alert(`Error: ${error1}`);
                error = error1;
            }
        } catch (err) {
            alert('An error occurred. Please try again.');
            console.error(err);
        }
    };
</script>

<!-- Apply Bootstrap styles here -->
<form on:submit|preventDefault={handleRegister} class="form-signin w-100 p-4 m-auto">
    <h2 class="h3 mb-3 font-weight-normal text-center">Register</h2>

    <!-- Username -->
    <div class="form-group">
        <label for="username">Username</label>
        <input
            type="text"
            id="username"
            class="form-control"
            bind:value={Username}
            placeholder="Enter username"
            required
        />
    </div>

    <!-- Email -->
    <div class="form-group mt-2">
        <label for="email">Email</label>
        <input
            type="email"
            id="email"
            class="form-control"
            bind:value={Email}
            placeholder="Enter email"
            required
        />
    </div>

    <!-- Password -->
    <div class="form-group mt-2">
        <label for="password">Password</label>
        <input
            type="password"
            id="password"
            class="form-control  {error && !/^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{16,100}$/.test(Password) ? 'is-invalid' : ''}"
            bind:value={Password}
            placeholder="Enter password"
            required
        />
        {#if error && !/^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{16,100}$/.test(Password)}
            <div class="invalid-feedback">
                Password must be 16-100 characters long, with at least one uppercase letter, one number, and one special character.
            </div>
        {/if}
    </div>

    <!-- Submit Button with spacing -->
    <button type="submit" class="btn btn-primary w-100 mt-4">Register</button>
    <div class="mt-4">
        <label for="login-link" class="text-center">Already have an account?</label>
        <button type="button" class="btn btn-primary w-100" id="login-link" on:click={() => navigate('/login')}>Login</button>
    </div>
</form>

<style>
    form {
        max-width: 400px;
        margin: auto;
    }
</style>
