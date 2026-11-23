import React from 'react';
import Original from '@theme-original/BlogListPage';
import NoIndex from '@site/src/components/NoIndex';

// /blog is the one indexed blog listing page; its numbered pages are not indexed (rules.md "Docs blog listing
// indexing", work item 26112202 D-016).
export default function BlogListPageWrapper(props) {
    return (
        <>
            {props.metadata.page > 1 && <NoIndex />}
            <Original {...props} />
        </>
    );
}
