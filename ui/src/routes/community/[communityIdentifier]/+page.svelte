<script lang="ts">
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<p>{data.community?.description}</p>
