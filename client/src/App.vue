<template>
    <div id="app">
        <router-link :to="{ name: 'Home' }">Home</router-link>
        <router-view></router-view>
    </div>
</template>

<script>
    import { log } from 'semanticLink';

    export default {
        name: 'app',
    };
</script>
