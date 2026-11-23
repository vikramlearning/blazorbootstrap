import React from 'react';
import Original from '@theme-original/BlogArchivePage';
import NoIndex from '@site/src/components/NoIndex';

// Blog listing pages other than /blog are not indexed (rules.md "Docs blog listing indexing", work item 26112202 D-016).
export default function BlogArchivePageWrapper(props) {
    return (
        <>
            <NoIndex />
            <Original {...props} />
        </>
    );
}
