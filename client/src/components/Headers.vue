<template>
    <b-card :title="title">
        <b-container fluid>
            <b-row class="row" v-for="(value, key) in headers">
                <b-col cols="2" class="text-right header-key">{{key}}</b-col>
                <b-col cols="10">{{value}}</b-col>
            </b-row>
        </b-container>
    </b-card>
</template>

<script>
    export default {
        name: "Headers",
        props: {
            title: {type: String, required: true},
            headers: {type: Object, required: true}
        }
    }
</script>

<style scoped>
    .header-key {
        color: blue;
        font-weight: bold;
    }
</style>