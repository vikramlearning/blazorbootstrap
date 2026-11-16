using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

/// <summary>
/// The simulated assistant's replies: fixed Markdown templates, two variants each, chosen by keywords in the prompt. No
/// model is called (AC-19). Replies never quote the person's text or file names, because the <c>Markdown</c> component
/// renders raw HTML; code inside the templates is already HTML-escaped (<c>&amp;lt;</c>), since its code blocks aren't.
/// </summary>
public static class AiReplies
{
    #region Fields and Constants

    /// <summary>The reply when a prompt mentions a blocked term.</summary>
    public const string PolicyKey = "policy";

    private static readonly Dictionary<string, string[]> templates = new()
    {
        ["greeting"] = new[]
        {
            """
            Hi! I can help you **write**, **summarize**, **plan**, and **code**. A few things to try:

            - "Draft an email to the design team about the launch date"
            - "Write a SQL query for monthly churn"
            - "Explain what a race condition is"

            You can also start from a prompt in the library.
            """,
            """
            Hello again. What are you working on today? Paste some text to summarize, describe a task to plan, or ask about code. I keep the whole conversation in mind, so you can follow up.
            """,
        },
        ["code-csharp"] = new[]
        {
            """
            Here's a safe way to handle a value that might be missing, with a guard clause and a clear error:

            ```csharp
            public Order GetOrder(int id)
            {
                var order = orders.FirstOrDefault(o => o.Id == id);

                if (order is null)
                    throw new KeyNotFoundException($"Order {id} was not found.");

                return order;
            }
            ```

            **Why it helps**

            1. The null check sits right where the value is read, so the failure points at the cause.
            2. The message names the missing ID, which makes logs useful.
            3. Callers that expect "not found" can catch `KeyNotFoundException` instead of a null reference.

            If missing orders are normal, return `Order?` instead and let callers decide.
            """,
            """
            A common fix for a null reference is to make the possibly-missing value explicit in the type:

            ```csharp
            public string DisplayName(Customer? customer) =>
                customer?.Name ?? "Guest";

            var names = new List&lt;string&gt;();
            names.Add(DisplayName(null));
            ```

            - `?.` stops when `customer` is null instead of throwing.
            - `??` supplies a fallback.
            - Turning on nullable reference types makes the compiler warn about the risky spots before they run.
            """,
        },
        ["code-js"] = new[]
        {
            """
            Here's a small debounce helper, so a search box doesn't send a request on every keystroke:

            ```javascript
            function debounce(callback, delay) {
              let timer;
              return (value) => {
                clearTimeout(timer);
                timer = setTimeout(() => callback(value), delay);
              };
            }

            const search = debounce((text) => fetchResults(text), 300);
            input.addEventListener('input', (event) => search(event.target.value));
            ```

            The request goes out 300 ms after the person stops typing. Lower the delay for a snappier feel, raise it to save requests.
            """,
            """
            To load data without blocking the page, use `async` and `await` with error handling:

            ```javascript
            async function loadOrders() {
              try {
                const response = await fetch('/api/orders');
                if (!response.ok) throw new Error('Request failed: ' + response.status);
                return await response.json();
              } catch (error) {
                console.error(error);
                return [];
              }
            }
            ```

            Returning an empty list keeps the page usable when the request fails; show a message so people know why the list is empty.
            """,
        },
        ["sql"] = new[]
        {
            """
            Here's monthly churn: customers who were active last month but not this month, as a share of last month's customers.

            ```sql
            WITH Monthly AS (
              SELECT DISTINCT CustomerId, DATEFROMPARTS(YEAR(OrderedAt), MONTH(OrderedAt), 1) AS Month
              FROM Orders
            )
            SELECT Cur.Month,
                   COUNT(Prev.CustomerId) AS ActiveLastMonth,
                   COUNT(Prev.CustomerId) - COUNT(Still.CustomerId) AS Churned
            FROM (SELECT DISTINCT Month FROM Monthly) AS Cur
            JOIN Monthly AS Prev ON Prev.Month = DATEADD(MONTH, -1, Cur.Month)
            LEFT JOIN Monthly AS Still ON Still.CustomerId = Prev.CustomerId AND Still.Month = Cur.Month
            GROUP BY Cur.Month
            ORDER BY Cur.Month;
            ```

            Divide `Churned` by `ActiveLastMonth` for the rate. Check the result against a month you already know before you share it.
            """,
            """
            A quick way to find your top customers by revenue this year:

            ```sql
            SELECT TOP (10) C.Name, SUM(O.Total) AS Revenue
            FROM Customers AS C
            JOIN Orders AS O ON O.CustomerId = C.Id
            WHERE O.OrderedAt &gt;= '2026-01-01'
            GROUP BY C.Name
            ORDER BY Revenue DESC;
            ```

            Add an index on `Orders (CustomerId, OrderedAt)` if this runs slowly.
            """,
        },
        ["email"] = new[]
        {
            """
            Here's a draft you can adjust:

            > **Subject:** Launch date moved to December 2
            >
            > Hi team,
            >
            > We're moving the launch to **Tuesday, December 2** to give QA two more days on the checkout flow. Design hand-off stays on Friday.
            >
            > What I need from you: updated mockups for the empty states by Thursday.
            >
            > Thanks, and shout if this causes problems.

            Want it shorter, more formal, or with a bullet list of changes?
            """,
            """
            A short, friendly version:

            > **Subject:** Quick update on the launch
            >
            > Hi all, a quick heads-up: the launch is now on **December 2**. Nothing changes for the design hand-off. Questions? Reply here or find me after stand-up.

            I kept it under 60 words so it reads well on a phone.
            """,
        },
        ["plan"] = new[]
        {
            """
            ## A four-week plan

            **Week 1: Decide**
            - Agree on the one goal and how you'll measure it.
            - List the risks and who owns each.

            **Week 2: Build**
            - Ship the smallest version to a few internal users.
            - Collect feedback daily.

            **Week 3: Harden**
            - Fix what blocked people; cut what nobody used.
            - Write the help article and the release note.

            **Week 4: Launch**
            - Roll out in stages and watch the numbers.
            - Hold a short review: what to keep, what to change.

            Tell me your deadline and team size and I'll adjust the timing.
            """,
            """
            ## Plan in three steps

            1. **Clarify the outcome.** One sentence: who benefits, and how you'll know.
            2. **Make it small.** Pick the first slice you can finish in a week.
            3. **Check in often.** A 15-minute review every two days catches problems early.

            > Tip: write down what you'll *not* do this time. It keeps the plan honest.
            """,
        },
        ["summary"] = new[]
        {
            """
            **Summary**

            - The main point: the team agreed to ship a smaller first version on time rather than a complete one late.
            - Open questions: who owns the onboarding emails, and whether pricing changes in this release.
            - Next steps: design hand-off on Friday; a go/no-go review the following Wednesday.

            Want this as a short paragraph for an update instead?
            """,
            """
            **In one paragraph:** the proposal trades scope for speed: a smaller launch now, with the rest following in two monthly releases. The main risk is support load in the first week, which the team plans to cover with an FAQ and a temporary rota.
            """,
        },
        ["explain"] = new[]
        {
            """
            **In short:** a race condition happens when two things run at the same time and the result depends on which finishes first.

            **An everyday example:** two people edit the same document offline; whoever syncs last silently overwrites the other.

            **How to avoid it**
            - Let only one thing change shared data at a time (a lock or a queue).
            - Make updates check what they expect first ("only save if the version is still 7").
            - Prefer data that doesn't change once written.

            Want a code example in a specific language?
            """,
            """
            Think of it like this: **caching** keeps a copy of something that's slow to get, so the next request is fast.

            - **Good for:** data that's read often and changes rarely.
            - **Watch out for:** stale copies. Decide how long a copy may live, or clear it when the source changes.
            - **Rule of thumb:** cache at the edge closest to the person, and measure before and after.
            """,
        },
        ["attachments"] = new[]
        {
            """
            Thanks for the files. **In this demo I can't read their contents**: only their names and sizes are kept, and nothing is uploaded.

            In a real workspace I'd summarize each file and answer questions about it. Meanwhile, you can paste a short excerpt here and I'll work with that.
            """,
            """
            I see your attachments listed above. This demo keeps only their names and sizes; it doesn't open them. Paste the part you care about, and I'll summarize or rewrite it.
            """,
        },
        [PolicyKey] = new[]
        {
            """
            I can't help with that request: it matches a topic your **workspace policy** doesn't allow. If you think this is a mistake, ask a workspace admin to review the blocked terms.
            """,
            """
            Sorry, that's outside what this workspace allows me to discuss. Try rephrasing the request without the restricted topic, or contact a workspace admin.
            """,
        },
        ["fallback"] = new[]
        {
            """
            Here's how I'd approach it:

            1. **Pin down the goal.** What would a great answer let you do next?
            2. **Gather the facts** you already have, and note what's missing.
            3. **Draft, then cut.** A first version that's half as long is usually clearer.

            Tell me a bit more (audience, length, deadline) and I'll make it specific.
            """,
            """
            Good question. A few angles to consider:

            - **Who is it for?** The right answer for a customer differs from one for your team.
            - **What's the constraint?** Time, budget, or risk usually decides it.
            - **What would change your mind?** Name it, then check it first.

            Share an example and I'll work through it with you.
            """,
        },
    };

    private static readonly (string Key, string[] Words)[] keywords =
    {
        ("code-csharp", new[] { "c#", "csharp", ".net", "dotnet", "null reference", "nullreference", "exception", "linq", "blazor" }),
        ("code-js", new[] { "javascript", " js", "react", "debounce", "fetch", "typescript", "node" }),
        ("sql", new[] { "sql", "query", "churn", "database", "select ", "join" }),
        ("email", new[] { "email", "e-mail", "mail to", "write to the", "message to" }),
        ("plan", new[] { "plan", "roadmap", "launch", "steps", "timeline", "schedule" }),
        ("summary", new[] { "summarize", "summarise", "summary", "tl;dr", "recap" }),
        ("explain", new[] { "explain", "what is", "what's a", "why does", "how does", "difference between" }),
        ("greeting", new[] { "hello", "hi ", "hey", "good morning" }),
    };

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns whether a stored reply key and variant name a template.</summary>
    public static bool IsKnown(string? key, int variant) => key is not null && templates.TryGetValue(key, out var set) && variant >= 0 && variant < set.Length;

    /// <summary>Picks the reply template for a prompt: policy refusal, attachments, a keyword match, or the fallback.</summary>
    public static string Pick(string text, bool hasAttachments, bool isBlocked)
    {
        if (isBlocked)
            return PolicyKey;

        var lower = " " + text.ToLowerInvariant() + " ";

        foreach (var (key, words) in keywords)
        {
            if (words.Any(lower.Contains))
                return key;
        }

        return hasAttachments ? "attachments" : "fallback";
    }

    /// <summary>Returns a template's Markdown, with a closing line naming the model.</summary>
    public static string Render(string key, int variant, AiModel? model)
    {
        var body = templates.TryGetValue(key, out var set) ? set[Math.Clamp(variant, 0, set.Length - 1)] : templates["fallback"][0];

        return model is null || key == PolicyKey ? body : body + $"\n\n*Answered by {model.Name}. Replies in this demo are prewritten examples.*";
    }

    /// <summary>Returns how many variants a template has.</summary>
    public static int VariantCount(string key) => templates.TryGetValue(key, out var set) ? set.Length : 1;

    #endregion

    #endregion
}
