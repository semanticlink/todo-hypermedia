<template>
    <div id="app">
        <b-link :to="{ name: 'Home' }">Home</b-link>
        <b-link :to="{ name: 'Roam' }">Roamer</b-link>
        <router-view></router-view>
    </div>
</template>

<script>
    export default {
        name: 'app',
    };
</script>
