import { data } from "jquery";
import { indexApi } from "./indexApi";

export const crawlingJobApi = indexApi.injectEndpoints({
  endpoints: (builder) => ({
    addCrawlingJob: builder.mutation<
      authTypes.apiResponse,
      ApiTypes.AddCrawlingJobParams
    >({
      query: (data) => ({
        method: "POST",
        url: "Crawler/AddJob",
        body: data,
      }),
      invalidatesTags: ["CrawlingJob"],
    }),
    getCrawlingJobs: builder.query<
      ApiTypes.GetCrawlingJobProps,
      Global.SearchParams
    >({
      query: (data) => ({
        url: "Crawler/GetCrawlingJobs",
        params: data,
      }),
      providesTags: ["CrawlingJob"],
    }),
    getCrawlingJobResponse: builder.query<
      ApiTypes.GetCrawlingJobResponse,
      number
    >({
      query: (data) => ({
        url: `Crawler/GetResponseForJobId/${data}`,
        method: "GET",
      }),
      providesTags: ["CrawlingJob"],
    }),
    triggerJob: builder.mutation<authTypes.apiResponse, number>({
      query: (data) => ({
        url: `Crawler/TriggerJob/${data}`,
        method: "POST",
      }),
      invalidatesTags: ["CrawlingJob"],
    }),
    deleteJob: builder.mutation<authTypes.apiResponse, number>({
      query: (data) => ({
        url: `Crawler/RemoveJob/${data}`,
        method: "DELETE",
      }),
      invalidatesTags: ["CrawlingJob"],
    }),
    getFormData: builder.query<ApiTypes.GetFormData, number>({
      query: (data) => ({
        url: `Crawler/GetFormByJobId/${data}`,
      }),
      providesTags: ["CrawlingJob"],
    }),
    editJob: builder.mutation<
      authTypes.apiResponse,
      ApiTypes.EditCrawlingJobParams
    >({
      query: (data) => ({
        url: `Crawler/EditJob/${data.id}`,
        method: "PUT",
        body: data.obj,
      }),
    }),
  }),
});

export const {
  useAddCrawlingJobMutation,
  useGetCrawlingJobsQuery,
  useGetCrawlingJobResponseQuery,
  useTriggerJobMutation,
  useDeleteJobMutation,
  useGetFormDataQuery,
  useEditJobMutation,
} = crawlingJobApi;
