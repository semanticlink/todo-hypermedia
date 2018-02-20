<template>
    <div id="app">
        <router-link :to="{ name: 'Home' }">Home</router-link>
        <router-view></router-view>
    </div>
</template>

<script>
    export default {
        name: 'app',
    };
</script>
