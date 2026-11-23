import React from 'react';
import Head from '@docusaurus/Head';

// Keeps a page out of search results (rules.md "Docs blog listing indexing"). The sitemap plugin also leaves out every
// page whose head carries this tag, so listing pages that use it drop out of sitemap.xml without a separate list.
export default function NoIndex() {
    return (
        <Head>
            <meta name="robots" content="noindex" />
        </Head>
    );
}
